import { useCallback, useEffect, useState } from 'react';
import * as contentsApi from '../api/contents';
import type { ContentDto, CurrentUser, MarkerDto } from '../types';
import { isAdmin } from '../store/authStore';

interface Props {
  marker: MarkerDto | null;
  currentUser: CurrentUser | null;
  onEdit: (marker: MarkerDto) => void;
  onDelete: (marker: MarkerDto) => void;
  onChanged: () => void;
}

export default function DetailPanel({ marker, currentUser, onEdit, onDelete, onChanged }: Props) {
  const [contents, setContents] = useState<ContentDto[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [uploading, setUploading] = useState(false);

  const canManage = marker !== null && currentUser !== null && (isAdmin(currentUser) || marker.userId === currentUser.id);

  const loadContents = useCallback(async (markerId: string) => {
    try {
      setError(null);
      setContents(await contentsApi.getByMarker(markerId));
    } catch (e) {
      setContents([]);
      setError((e as Error).message ?? 'Не удалось загрузить контент');
    }
  }, []);

  useEffect(() => {
    setContents([]);
    setError(null);
    if (marker) {
      void loadContents(marker.id);
    }
  }, [marker, loadContents]);

  if (!marker) {
    return (
      <aside className="detail-panel">
        <p style={{ color: '#6b7280' }}>
          Выберите метку на карте или в списке, чтобы увидеть подробности.
        </p>
      </aside>
    );
  }

  const handleFile = async (file: File | undefined) => {
    if (!file || !canManage) return;
    setUploading(true);
    setError(null);
    try {
      await contentsApi.upload(marker.id, file);
      await loadContents(marker.id);
      onChanged();
    } catch (e) {
      setError((e as Error).message ?? 'Ошибка загрузки файла');
    } finally {
      setUploading(false);
    }
  };

  const handleDeleteContent = async (id: string) => {
    if (!canManage) return;
    try {
      await contentsApi.deleteContent(id);
      await loadContents(marker.id);
      onChanged();
    } catch (e) {
      setError((e as Error).message ?? 'Ошибка удаления');
    }
  };

  const imageUrls = contents.map((c) => c.url).filter((u): u is string => !!u);

  return (
    <aside className="detail-panel">
      <h2>{marker.name}</h2>
      <span className={`badge ${marker.isPublic ? 'public' : 'private'}`}>
        {marker.isPublic ? 'публичная' : 'приватная'}
      </span>

      <div className="field">
        <div className="label">Описание</div>
        <div>{marker.description || '—'}</div>
      </div>
      <div className="field">
        <div className="label">Категория</div>
        <div>{marker.categoryName || '—'}</div>
      </div>
      <div className="field">
        <div className="label">Координаты</div>
        <div>
          {marker.latitude?.toFixed(5) ?? '—'}, {marker.longitude?.toFixed(5) ?? '—'}
        </div>
      </div>
      <div className="field">
        <div className="label">Создана</div>
        <div>{new Date(marker.createdAt).toLocaleString('ru-RU')}</div>
      </div>

      {error && <div className="error">{error}</div>}

      <div className="field">
        <div className="label">Контент ({contents.length})</div>
        {imageUrls.length > 0 && (
          <div className="content-grid">
            {imageUrls.map((url) => (
              <a key={url} href={url} target="_blank" rel="noreferrer">
                <img src={url} alt="" loading="lazy" />
              </a>
            ))}
          </div>
        )}
        {contents
          .filter((c) => !c.url)
          .map((c) => (
            <div key={c.id} className="content-item">
              <span>{c.path}</span>
              {canManage && (
                <button onClick={() => handleDeleteContent(c.id)} title="Удалить">
                  ✕
                </button>
              )}
            </div>
          ))}
      </div>

      {canManage && (
        <>
          <div className="field">
            <div className="label">Загрузить файл</div>
            <input
              type="file"
              disabled={uploading}
              onChange={(e) => handleFile(e.target.files?.[0])}
            />
          </div>
          <div className="actions">
            <button className="primary" onClick={() => onEdit(marker)}>
              Редактировать
            </button>
            <button className="danger" onClick={() => onDelete(marker)}>
              Удалить
            </button>
          </div>
        </>
      )}
    </aside>
  );
}