import axios from 'axios';

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
  withCredentials: true,
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('access_token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;
      try {
        const retryResponse = await api.post('/refresh');
        const newToken = retryResponse.data.access_token;
        localStorage.setItem('access_token', newToken);
        return api(originalRequest);
      } catch {
        localStorage.removeItem('access_token');
        window.location.reload();
        return Promise.reject(error);
      }
    }
    return Promise.reject(error);
  },
);

export default api;
