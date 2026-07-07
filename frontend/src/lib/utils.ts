import { type ClassValue, clsx } from 'clsx';
import { twMerge } from 'tailwind-merge';
import { AxiosError } from 'axios';
import type { ErrorMessage } from '@/types';

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

export function isValidHttpUrl(value?: string) {
  let url;

  if (!value) return false;

  try {
    url = new URL(value);
  } catch (_) {
    void _;
    return false;
  }

  return url.protocol === 'http:' || url.protocol === 'https:';
}

export function safeAtob(value: string | null | undefined, fallback = ''): string {
  if (value == null) return fallback;
  try {
    return atob(value);
  } catch {
    return fallback;
  }
}

export function safeBtoa(value: string | null | undefined, fallback = ''): string {
  if (value == null) return fallback;
  try {
    return btoa(value);
  } catch {
    return fallback;
  }
}

export function decodeJWT<T>(token: string): T {
  const payloadBase64 = token.split('.')[1];
  const decodedPayload = safeAtob(payloadBase64);
  return JSON.parse(decodedPayload);
}

export function extractApiErrorMessage(err: unknown, fallback: string): string {
  const axiosError = err as AxiosError<ErrorMessage>;
  if (axiosError?.code === AxiosError.ERR_NETWORK) {
    return 'Network Error. Either you are not connected, or the server is offline.';
  }
  return axiosError?.response?.data?.message || axiosError?.message || fallback;
}

export function readableSize(bytes: number): string {
  if (bytes === 0) return '0 B';
  const sizes = ['B', 'KB', 'MB', 'GB'];
  const i = Math.floor(Math.log(bytes) / Math.log(1024));
  return (bytes / Math.pow(1024, i)).toFixed(2) + ' ' + sizes[i];
}

export const mapMediaType: Record<string, string> = {
  image:
    'image/jpeg, image/jpg, image/png, image/gif, image/webp, image/avif, image/svg+xml, image/tiff, image/bmp, image/x-icon, image/vnd.microsoft.icon',
  video:
    'video/mp4, video/webm, video/ogg, video/avi, video/mov, video/mpeg, video/3gpp, video/3gpp2, video/quicktime, video/x-msvideo',
  audio: 'audio/mpeg, audio/mp3, audio/wav, audio/ogg, audio/flac, audio/aac, audio/x-m4a',
  raw:
    // "raw" covers things that are not image/video (docs, archives, etc.)
    'application/pdf, application/zip, application/gzip, application/x-rar-compressed, application/msword, application/vnd.openxmlformats-officedocument.wordprocessingml.document, application/vnd.ms-excel, application/vnd.openxmlformats-officedocument.spreadsheetml.sheet, application/vnd.ms-powerpoint, application/vnd.openxmlformats-officedocument.presentationml.presentation, text/plain, application/json, application/xml, application/vnd.adobe.photoshop',
  _: '',
};
