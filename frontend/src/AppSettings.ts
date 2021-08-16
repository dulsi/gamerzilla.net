export const server = 'http://' + window.location.hostname + ':5000';

export const webAPIUrl = `${server}/api`;

// For testing it goes directly against the backend.
// For production it should just be /api.
export const relativeAPIUrl = `${server}/api`;