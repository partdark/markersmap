import client from './client';
import type { ContentDto } from '../types';

export const getByMarker = (markerId: string) =>
  client.get<ContentDto[]>(`/contents/by-marker/${markerId}`).then((r) => r.data);

export const upload = (markerId: string, file: File) => {
  const formData = new FormData();
  formData.append('markerId', markerId);
  formData.append('file', file);
  return client.post<ContentDto>('/contents/upload', formData).then((r) => r.data);
};

export const deleteContent = (id: string) => client.delete(`/contents/${id}`);