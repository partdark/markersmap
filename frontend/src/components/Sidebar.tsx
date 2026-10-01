import { useState } from 'react';
import type { CategoryDto, MarkerDto } from '../types';

interface Props {
  markers: MarkerDto[];
  search: string;
  onSearchChange: (value: string) => void;
  categories: string[];
  categoryFilter: string;
  onCategoryChange: (value: string) => void;
  selectedId: string | null;
  onSelect: (id: string) => void;
  // Панель администратора
  admin: boolean;
  allCategories: CategoryDto[];
  onCreateCategory: (name: string) => void;
  onDeleteCategory: (id: string) => void;
}

export default function Sidebar({
  markers,
  search,
  onSearchChange,
  categories,
  categoryFilter,
  onCategoryChange,
  selectedId,
  onSelect,
  admin,
  allCategories,
  onCreateCategory,
  onDeleteCategory,
}: Props) {
  const [newCategory, setNewCategory] = useState('');

  return (
    <aside className="sidebar">
      <div className="sidebar-toolbar">
        <input
          type="text"
          placeholder="Поиск по названию…"
          value={search}
          onChange={(e) => onSearchChange(e.target.value)}
        />
        <select value={categoryFilter} onChange={(e) => onCategoryChange(e.target.value)}>
          <option value="">Все категории</option>
          {categories.map((c) => (
            <option key={c} value={c}>
              {c}
            </option>
          ))}
        </select>
        <span style={{ fontSize: 12, color: '#6b7280' }}>Найдено: {markers.length}</span>
      </div>

      {admin && (
        <div className="sidebar-toolbar" style={{ borderBottom: '1px solid #e5e7eb' }}>
          <strong style={{ fontSize: 13 }}>Категории (админ)</strong>
          {allCategories.length === 0 && (
            <span style={{ fontSize: 12, color: '#6b7280' }}>Категорий пока нет</span>
          )}
          <ul style={{ margin: 0, padding: 0, listStyle: 'none' }}>
            {allCategories.map((c) => (
              <li
                key={c.id}
                style={{
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'space-between',
                  padding: '4px 0',
                  fontSize: 14,
                }}
              >
                <span>{c.name}</span>
                <button
                  style={{ padding: '2px 8px', fontSize: 12 }}
                  onClick={() => onDeleteCategory(c.id)}
                  title="Удалить категорию"
                >
                  ✕
                </button>
              </li>
            ))}
          </ul>
          <div style={{ display: 'flex', gap: 6 }}>
            <input
              type="text"
              placeholder="Новая категория…"
              value={newCategory}
              onChange={(e) => setNewCategory(e.target.value)}
              onKeyDown={(e) => {
                if (e.key === 'Enter' && newCategory.trim()) {
                  onCreateCategory(newCategory.trim());
                  setNewCategory('');
                }
              }}
            />
            <button
              onClick={() => {
                if (newCategory.trim()) {
                  onCreateCategory(newCategory.trim());
                  setNewCategory('');
                }
              }}
            >
              Добавить
            </button>
          </div>
        </div>
      )}

      <div className="marker-list">
        {markers.length === 0 && <div style={{ padding: 12, color: '#6b7280' }}>Метки не найдены</div>}
        {markers.map((m) => (
          <div
            key={m.id}
            className={`marker-list-item${m.id === selectedId ? ' selected' : ''}`}
            onClick={() => onSelect(m.id)}
          >
            <div className="name">
              {m.name}
              <span className={`badge ${m.isPublic ? 'public' : 'private'}`}>
                {m.isPublic ? 'публичн.' : 'приватн.'}
              </span>
            </div>
            <div className="meta">
              {m.categoryName}
              {m.latitude != null && m.longitude != null
                ? ` · ${m.latitude.toFixed(4)}, ${m.longitude.toFixed(4)}`
                : ''}
            </div>
          </div>
        ))}
      </div>
    </aside>
  );
}