import { useCallback, useEffect, useMemo, useState } from 'react';
import MapView, { type MapActions } from '../components/MapView';
import Sidebar from '../components/Sidebar';
import DetailPanel from '../components/DetailPanel';
import AddMarkerModal from '../components/AddMarkerModal';
import * as markersApi from '../api/markers';
import * as categoriesApi from '../api/categories';
import { useAuthStore, isAdmin } from '../store/authStore';
import type { CategoryDto, MarkerDto } from '../types';

export default function MapPage() {
  const { token, user } = useAuthStore();
  const [markers, setMarkers] = useState<MarkerDto[]>([]);
  const [categories, setCategories] = useState<CategoryDto[]>([]);
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [search, setSearch] = useState('');
  const [categoryFilter, setCategoryFilter] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState<MarkerDto | null>(null);
  const [clickPosition, setClickPosition] = useState<{ lat: number; lng: number } | null>(null);

  const isAuthed = !!token;

  const loadMarkers = useCallback(async () => {
    try {
      setError(null);
      const data = isAuthed ? await markersApi.getMarkers() : await markersApi.getPublicMarkers();
      setMarkers(data);
      setSelectedId((prev) => (prev && data.some((m) => m.id === prev) ? prev : null));
    } catch (e) {
      setError((e as Error).message ?? 'Не удалось загрузить метки');
    }
  }, [isAuthed]);

  useEffect(() => {
    void loadMarkers();
  }, [loadMarkers]);

  const loadCategories = useCallback(() => {
    if (!isAuthed) {
      setCategories([]);
      return;
    }
    categoriesApi
      .getCategories()
      .then(setCategories)
      .catch(() => setCategories([]));
  }, [isAuthed]);

  useEffect(() => {
    loadCategories();
  }, [loadCategories]);

  const handleCreateCategory = useCallback(
    (name: string) => {
      categoriesApi
        .createCategory(name)
        .then(() => loadCategories())
        .catch((e) => setError((e as Error).message ?? 'Не удалось создать категорию'));
    },
    [loadCategories],
  );

  const handleDeleteCategory = useCallback(
    (id: string) => {
      if (!window.confirm('Удалить категорию?')) return;
      categoriesApi
        .deleteCategory(id)
        .then(() => {
          loadCategories();
          void loadMarkers();
        })
        .catch((e) => setError((e as Error).message ?? 'Не удалось удалить категорию'));
    },
    [loadCategories, loadMarkers],
  );

  const selected = markers.find((m) => m.id === selectedId) ?? null;

  const filtered = useMemo(() => {
    const q = search.trim().toLowerCase();
    return markers.filter((m) => {
      if (categoryFilter && m.categoryName !== categoryFilter) return false;
      if (q && !m.name.toLowerCase().includes(q)) return false;
      return true;
    });
  }, [markers, search, categoryFilter]);

  const categoriesFromMarkers = useMemo(
    () => Array.from(new Set(markers.map((m) => m.categoryName).filter(Boolean))).sort(),
    [markers],
  );

  const canCreate = isAuthed;
  const canManage = (m: MarkerDto) => isAuthed && user !== null && (isAdmin(user) || m.userId === user.id);

  const actions: MapActions = useMemo(
    () => ({
      selectMarker: (id) => setSelectedId(id),
      editMarker: (id) => {
        const m = markers.find((x) => x.id === id);
        if (m && canManage(m)) {
          setEditing(m);
          setModalOpen(true);
        }
      },
      deleteMarker: (id) => {
        const m = markers.find((x) => x.id === id);
        if (m && canManage(m) && window.confirm(`Удалить метку «${m.name}»?`)) {
          void markersApi
            .deleteMarker(id)
            .then(() => loadMarkers())
            .catch((e) => setError((e as Error).message ?? 'Ошибка удаления'));
        }
      },
    }),
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [markers, isAuthed, user],
  );

  const handleMapClick = useCallback((lat: number, lng: number) => {
    setEditing(null);
    setClickPosition({ lat, lng });
    setModalOpen(true);
  }, []);

  const handleEdit = (m: MarkerDto) => {
    setEditing(m);
    setClickPosition(null);
    setModalOpen(true);
  };

  const handleDelete = (m: MarkerDto) => {
    actions.deleteMarker(m.id);
  };

  return (
    <div className="app-body">
      <Sidebar
        markers={filtered}
        search={search}
        onSearchChange={setSearch}
        categories={categoriesFromMarkers}
        categoryFilter={categoryFilter}
        onCategoryChange={setCategoryFilter}
        selectedId={selectedId}
        onSelect={setSelectedId}
        admin={isAdmin(user)}
        allCategories={categories}
        onCreateCategory={handleCreateCategory}
        onDeleteCategory={handleDeleteCategory}
      />

      <div className="map-container">
        {error && <div style={{ position: 'absolute', zIndex: 1000, top: 8, left: 8 }} className="error">{error}</div>}
        {canCreate && (
          <button
            style={{ position: 'absolute', zIndex: 1000, top: 8, right: 8 }}
            className="primary"
            onClick={() => {
              setEditing(null);
              setClickPosition(null);
              setModalOpen(true);
            }}
          >
            + Добавить метку
          </button>
        )}
        <MapView
          markers={markers}
          selectedId={selectedId}
          canCreate={canCreate}
          actions={actions}
          onMapClick={handleMapClick}
        />
      </div>

      <DetailPanel
        marker={selected}
        currentUser={user}
        onEdit={handleEdit}
        onDelete={handleDelete}
        onChanged={() => loadMarkers()}
      />

      <AddMarkerModal
        open={modalOpen}
        marker={editing}
        position={clickPosition}
        categories={categories}
        onClose={() => setModalOpen(false)}
        onSaved={() => loadMarkers()}
      />
    </div>
  );
}