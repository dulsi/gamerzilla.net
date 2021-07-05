export const server = 'http://' + window.location.hostname + ':5000';

export const webAPIUrl = `${server}/api/gamerzilla`;

// For testing it goes directly against the backend.
// For production it should just be /api/gamerzilla.
export const relativeAPIUrl = `${server}/api/gamerzilla`;