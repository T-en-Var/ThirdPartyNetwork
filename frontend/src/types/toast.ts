export type ToastType = 'success' | 'error' | 'info' | 'warning';

export interface Toast {
  id: string;
  message: string;
  type?: ToastType;
  duration?: number;
}

export interface NewToast {
  id?: string;
  message: string;
  type?: ToastType;
  duration?: number;
}

export type ToastList = Toast[];
