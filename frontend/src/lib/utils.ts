import { type ClassValue, clsx } from 'clsx'
import { twMerge } from 'tailwind-merge'

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
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

  return url.protocol === "http:" || url.protocol === "https:";
}
