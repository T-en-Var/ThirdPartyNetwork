const API_URL = import.meta.env.VITE_API_BASE_URL;

function getApiBaseUrl() {
  if (window.location.hostname !== 'localhost') {
    return `https://api.${window.location.hostname}/`;
  }
  return API_URL;
}

export const API_BASE_URL = getApiBaseUrl();
