import client from './client';
import type { MarkerDto, MarkerInput } from '../types';

export const getMarkers = () => client.get<MarkerDto[]>('/markers').then((r) => r.data);
export const getPublicMarkers = () => client.get<MarkerDto[]>('/markers/public').then((r) => r.data);
export const getMarker = (id: string) => client.get<MarkerDto>(`/markers/${id}`).then((r) => r.data);

export const createMarker = (input: MarkerInput) =>
  client.post<MarkerDto>('/markers', input).then((r) => r.data);

export const updateMarker = (id: string, input: MarkerInput) =>
  client.put<MarkerDto>(`/markers/${id}`, input).then((r) => r.data);

export const deleteMarker = (id: string) => client.delete(`/markers/${id}`);