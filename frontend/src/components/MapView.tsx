import { useEffect, useRef } from 'react';
import * as Cesium from 'cesium';
import 'cesium/Build/Cesium/Widgets/widgets.css';
import type { MarkerDto } from '../types';


(window as unknown as { CESIUM_BASE_URL?: string }).CESIUM_BASE_URL = '/cesium';

export interface MapActions {
  selectMarker: (id: string) => void;
  editMarker: (id: string) => void;
  deleteMarker: (id: string) => void;
}

interface Props {
  markers: MarkerDto[];
  selectedId: string | null;
  canCreate: boolean;
  actions: MapActions;
  onMapClick: (lat: number, lng: number) => void;
}

const MARKER_PREFIX = 'marker:';

function escapeHtml(value: string): string {
  return value.replace(/&/g, '&').replace(/</g, '<').replace(/>/g, '>').replace(/"/g, '"');
}

export default function MapView({ markers, selectedId, canCreate, actions, onMapClick }: Props) {
  const containerRef = useRef<HTMLDivElement>(null);
  const viewerRef = useRef<Cesium.Viewer | null>(null);
  const clickRef = useRef({ canCreate, onMapClick });
  const actionsRef = useRef(actions);
  clickRef.current = { canCreate, onMapClick };
  actionsRef.current = actions;

  useEffect(() => {
    const container = containerRef.current;
    if (!container || viewerRef.current) return;

    const viewer = new Cesium.Viewer(container, {
      animation: false,
      baseLayerPicker: false,
      fullscreenButton: false,
      geocoder: false,
      homeButton: false,
      infoBox: false,
      navigationHelpButton: false,
      sceneModePicker: false,
      selectionIndicator: false,
      timeline: false,
      requestRenderMode: false,
    });

  
    viewer.imageryLayers.removeAll();
    viewer.imageryLayers.addImageryProvider(
      new Cesium.OpenStreetMapImageryProvider({ url: 'https://tile.openstreetmap.org/' }),
    );
    viewer.scene.globe.depthTestAgainstTerrain = false;


    viewer.camera.setView({
      destination: Cesium.Cartesian3.fromDegrees(60, 55, 5_000_000),
    });

   
    const handler = new Cesium.ScreenSpaceEventHandler(viewer.scene.canvas);
    handler.setInputAction((click: { position: Cesium.Cartesian2 }) => {
      const picked = viewer.scene.pick(click.position);
      const entity = picked?.id;

      if (entity instanceof Cesium.Entity && typeof entity.id === 'string' && entity.id.startsWith(MARKER_PREFIX)) {
        actionsRef.current.selectMarker(entity.id.slice(MARKER_PREFIX.length));
        return;
      }

      const cartesian = viewer.camera.pickEllipsoid(click.position, viewer.scene.globe.ellipsoid);
      if (cartesian && clickRef.current.canCreate) {
        const carto = Cesium.Cartographic.fromCartesian(cartesian);
        const lat = Cesium.Math.toDegrees(carto.latitude);
        const lng = Cesium.Math.toDegrees(carto.longitude);
        clickRef.current.onMapClick(lat, lng);
      }
    }, Cesium.ScreenSpaceEventType.LEFT_CLICK);

    viewerRef.current = viewer;

    return () => {
      handler.destroy();
      viewer.destroy();
      viewerRef.current = null;
    };
  }, []);

 
  useEffect(() => {
    const viewer = viewerRef.current;
    if (!viewer) return;

    const validIds = new Set(markers.map((m) => MARKER_PREFIX + m.id));

    viewer.entities.values
      .filter((e) => typeof e.id === 'string' && e.id.startsWith(MARKER_PREFIX))
      .forEach((e) => {
        if (!validIds.has(e.id)) viewer.entities.remove(e);
      });

    markers.forEach((m) => {
      if (m.latitude == null || m.longitude == null) return;
      const id = MARKER_PREFIX + m.id;
      if (viewer.entities.getById(id)) return;

      const color = m.isPublic
        ? Cesium.Color.fromCssColorString('#16a34a')
        : Cesium.Color.fromCssColorString('#f59e0b');

      viewer.entities.add({
        id,
        position: Cesium.Cartesian3.fromDegrees(m.longitude, m.latitude),
        point: {
          pixelSize: 14,
          color,
          outlineColor: Cesium.Color.WHITE,
          outlineWidth: 2,
          disableDepthTestDistance: Number.POSITIVE_INFINITY,
        },
        description: `<b>${escapeHtml(m.name)}</b><br/>${escapeHtml(m.categoryName)}`,
      });
    });
  }, [markers]);

 
  useEffect(() => {
    const viewer = viewerRef.current;
    const selected = markers.find((m) => m.id === selectedId);
    if (!viewer || !selected || selected.latitude == null || selected.longitude == null) return;

    const entity = viewer.entities.getById(MARKER_PREFIX + selected.id);
    const destination = Cesium.Cartesian3.fromDegrees(
      selected.longitude,
      selected.latitude,
      entity ? 1_000 : 1_000,
    );

   
    viewer.camera.flyTo({
      destination,
      orientation: {
        heading: 0,
        pitch: -Cesium.Math.PI_OVER_TWO,
        roll: 0,
      },
      duration: 2.0,
      easingFunction: Cesium.EasingFunction.QUADRATIC_IN_OUT,
    });
  }, [selectedId, markers]);

  return <div ref={containerRef} className="map" />;
}