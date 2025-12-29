const config = window.APP_CONFIG;

const isDev = import.meta.env.MODE === 'development';

let apiBase = '';

if (isDev) {
  apiBase = 'http://localhost:5000/api';
} else {
  apiBase = config?.apiUrl || '/api';
}

export const webAPIUrl = apiBase;

export const server = isDev
  ? 'http://localhost:5000'
  : config?.basePath === '/'
    ? ''
    : config?.basePath || '';

export {};

declare global {
  interface Window {
    APP_CONFIG?: {
      basePath: string;
      apiUrl: string;
    };
  }
}
