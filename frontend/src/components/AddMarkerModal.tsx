import { useEffect, useState } from 'react';
import * as markersApi from '../api/markers';
import type { MarkerDto } from '../types';

interface Props {
  open: boolean;
  marker: MarkerDto | null; // null — создание
  position: { lat: number; lng: number } | null;
  categories: { id: string; name: string }[];
  onClose: () => void;
  onSaved: () => void;
}

export default function AddMarkerModal({ open, marker, position, categories, onClose, onSaved }: Props) {
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [categoryId, setCategoryId] = useState('');
  const [isPublic, setIsPublic] = useState(true);
  const [latitude, setLatitude] = useState('');
  const [longitude, setLongitude] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    if (!open) return;
    if (marker) {
      setName(marker.name);
      setDescription(marker.description);
      setCategoryId(marker.categoryId);
      setIsPublic(marker.isPublic);
      setLatitude(marker.latitude?.toString() ?? '');
      setLongitude(marker.longitude?.toString() ?? '');
    } else {
      setName('');
      setDescription('');
      setCategoryId(categories[0]?.id ?? '');
      setIsPublic(true);
      setLatitude(position ? position.lat.toFixed(6) : '');
      setLongitude(position ? position.lng.toFixed(6) : '');
    }
    setError(null);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open]);

  if (!open) return null;

  const submit = async () => {
    const lat = parseFloat(latitude);
    const lng = parseFloat(longitude);
    if (!name.trim()) {
      setError('Укажите название');
      return;
    }
    if (!categoryId) {
      setError('Выберите категорию');
      return;
    }
    if (Number.isNaN(lat) || Number.isNaN(lng)) {
      setError('Укажите корректные координаты');
      return;
    }

    const input = {
      name: name.trim(),
      description: description.trim(),
      latitude: lat,
      longitude: lng,
      categoryId,
      isPublic,
    };

    setSaving(true);
    setError(null);
    try {
      if (marker) {
        await markersApi.updateMarker(marker.id, input);
      } else {
        await markersApi.createMarker(input);
      }
      onSaved();
      onClose();
    } catch (e) {
      const message =
        (e as { response?: { data?: { message?: string } } }).response?.data?.message ??
        (e as Error).message ??
        'Ошибка сохранения';
      setError(message);
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="modal-backdrop" onClick={onClose}>
      <div className="modal" onClick={(e) => e.stopPropagation()}>
        <h2>{marker ? 'Редактировать метку' : 'Новая метка'}</h2>
        <div className="form-row">
          <label>Название</label>
          <input value={name} onChange={(e) => setName(e.target.value)} />
        </div>
        <div className="form-row">
          <label>Описание</label>
          <textarea value={description} onChange={(e) => setDescription(e.target.value)} />
        </div>
        <div className="form-row">
          <label>Категория</label>
          <select value={categoryId} onChange={(e) => setCategoryId(e.target.value)}>
            {categories.length === 0 && <option value="">Нет категорий</option>}
            {categories.map((c) => (
              <option key={c.id} value={c.id}>
                {c.name}
              </option>
            ))}
          </select>
        </div>
        <div className="form-row">
          <label>Широта</label>
          <input value={latitude} onChange={(e) => setLatitude(e.target.value)} />
        </div>
        <div className="form-row">
          <label>Долгота</label>
          <input value={longitude} onChange={(e) => setLongitude(e.target.value)} />
        </div>
        <div className="form-row checkbox">
          <input type="checkbox" checked={isPublic} onChange={(e) => setIsPublic(e.target.checked)} />
          <label style={{ margin: 0 }}>Публичная (видят все)</label>
        </div>
        {error && <div className="error">{error}</div>}
        <div className="form-actions">
          <button onClick={onClose}>Отмена</button>
          <button className="primary" onClick={submit} disabled={saving}>
            {saving ? 'Сохранение…' : 'Сохранить'}
          </button>
        </div>
      </div>
    </div>
  );
}