import axios from 'axios';

const API_BASE_URL = 'https://localhost:7080/api'; // Update with your port

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Guest API calls
export const guestAPI = {
  getAll: () => api.get('/guests'),
  getById: (id) => api.get(`/guests/${id}`),
  register: (guestData) => api.post('/guests/register', guestData),
  login: (loginData) => api.post('/guests/login', loginData), // NEW!
  update: (id, guestData) => api.put(`/guests/${id}`, guestData),
  delete: (id) => api.delete(`/guests/${id}`),
  searchByEmail: (email) => api.get(`/guests/search?email=${email}`),
};

// Room API calls
export const roomAPI = {
  getAll: () => api.get('/rooms'),
  getById: (id) => api.get(`/rooms/${id}`),
  getAvailable: () => api.get('/rooms/available'),
  getByType: (type) => api.get(`/rooms/type/${type}`),
  create: (roomData) => api.post('/rooms', roomData),
  update: (id, roomData) => api.put(`/rooms/${id}`, roomData),
  delete: (id) => api.delete(`/rooms/{id}`),
};

// Booking API calls
export const bookingAPI = {
  getAll: () => api.get('/bookings'),
  getById: (id) => api.get(`/bookings/${id}`),
  getByGuest: (guestId) => api.get(`/bookings/guest/${guestId}`),
  getByStatus: (status) => api.get(`/bookings/status/${status}`),
  create: (bookingData) => api.post('/bookings', bookingData),
  updateStatus: (id, status) => api.put(`/bookings/${id}/status`, JSON.stringify(status)),
  cancel: (id) => api.delete(`/bookings/${id}`),
};

export const reviewAPI = {
  getAll: () => api.get('/reviews'),
  getByGuest: (guestId) => api.get(`/reviews/guest/${guestId}`),
  getStats: () => api.get('/reviews/stats'),
  create: (reviewData) => api.post('/reviews', reviewData),
  delete: (id) => api.delete(`/reviews/${id}`),
};


export default api;