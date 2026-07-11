import { API_BASE_URL } from '@/config';
import axios from 'axios';

const rawApi = axios.create({
  baseURL: API_BASE_URL,
  withCredentials: true,
});

export default rawApi;
