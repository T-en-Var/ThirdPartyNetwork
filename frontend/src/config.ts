const API_URL = import.meta.env.VITE_API_BASE_URL;

function getApiBaseUrl() {
    // If running in Docker/production, use relative URL
    if (window.location.hostname !== 'localhost') {
        console.log(`https://api.${window.location.hostname}/`)
        return `https://api.${window.location.hostname}/`;
    }
    // If in development mode, use the full URL
    return API_URL;
}

export const API_BASE_URL = getApiBaseUrl();
