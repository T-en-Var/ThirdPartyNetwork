const API_URL = import.meta.env.VITE_API_BASE_URL;

export const CLOUDINARY_CLOUD_NAME = import.meta.env.VITE_CLOUDINARY_CLOUD_NAME
export const CLOUDINARY_API_KEY = import.meta.env.VITE_CLOUDINARY_API_KEY
export const CLOUDINARY_API_SECRET = import.meta.env.VITE_CLOUDINARY_API_SECRET
export const CLOUDINARY_API_ENV_VAR = import.meta.env.VITE_CLOUDINARY_API_ENV_VAR

function getApiBaseUrl() {
    // If running in Docker/production, use relative URL
    if (window.location.hostname !== 'localhost' || !window.location.port.startsWith('51')) {
        console.log(`https://api.${window.location.hostname}/`)
        return `https://api.${window.location.hostname}/`;
    }
    // If in development mode, use the full URL
    return API_URL;
}

export const API_BASE_URL = getApiBaseUrl();
