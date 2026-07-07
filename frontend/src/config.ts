const API_URL = import.meta.env.VITE_API_BASE_URL;

function getApiBaseUrl() {
  return API_URL;
}

export const API_BASE_URL = getApiBaseUrl();
