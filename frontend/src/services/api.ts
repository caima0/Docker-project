import axios from 'axios';

const API_URL = (process.env.REACT_APP_API_URL || 'http://localhost:5000') + '/api';

const api = axios.create({
    baseURL: API_URL,
    headers: {
        'Content-Type': 'application/json',
    },
});

api.interceptors.request.use((config) => {
    const token = localStorage.getItem('token');
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

export interface ForgotPasswordData {
    email: string;
    newPassword: string;
}

export interface CurrencyRate {
    code: string;
    currency: string;
    bid: number;
    ask: number;
}

export const authService = {
    login: async (username: string, password: string) => {
        const response = await api.post('/Account/login', { username, password });
        return response.data;
    },
    register: async (username: string, email: string, password: string) => {
        const response = await api.post('/Account/register', { username, email, password });
        return response.data;
    },
    forgotPassword: async (data: ForgotPasswordData) => {
        const response = await api.post('/Account/forgot-password', data);
        return response.data;
    }
};

export const currencyService = {
    getRates: async () => {
        const response = await api.get('/Rate');
        return response.data;
    },
    convertCurrency: async (fromCurrency: string, toCurrency: string, amount: number) => {
        const response = await api.post('/Payment/create', {
            amount,
            currency: fromCurrency
        });
        return response.data;
    },
    updateRate: async (code: string, data: Partial<CurrencyRate>) => {
        const response = await api.put(`/Rate/${code}`, data);
        return response.data;
    },
    deleteRate: async (code: string) => {
        const response = await api.delete(`/Rate/${code}`);
        return response.data;
    },
    updateRatesFromNBP: async () => {
        const response = await api.post('/Rate/update-from-nbp');
        return response.data;
    }
};

export const balanceService = {
    getBalances: async () => {
        const response = await api.get('/Payment/balance');
        return response.data;
    },
    getBalanceHistory: async (currency?: string, page: number = 1, pageSize: number = 10) => {
        const params = new URLSearchParams();
        if (currency) params.append('currency', currency);
        params.append('page', page.toString());
        params.append('pageSize', pageSize.toString());
        const response = await api.get(`/Payment/balance/history?${params.toString()}`);
        return response.data;
    }
};

export default api; 