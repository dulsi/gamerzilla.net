export {};

declare global {
  interface Window {
    APP_CONFIG?: {
      basePath: string;
      apiUrl: string;
    };
  }
}
