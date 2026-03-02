import { type ReactNode, useState, useEffect } from 'react';
import { User, AuthContext } from './AuthContext';
import { getAuthToken, setAuthToken, removeAuthToken, authFetch } from '@/api/fetchUtils';


export function AuthProvider({ children }: { children: ReactNode; }) {
    const [user, setUser] = useState<User | null>(null);

    useEffect(() => {
        const hash = window.location.hash;
        const params = new URLSearchParams(hash.slice(1));
        const token = params.get('token');

        if (token) {
            setAuthToken(token);
            window.history.replaceState(null, '', window.location.pathname + window.location.search);
        }

        const storedToken = getAuthToken();
        if (!storedToken) {
            return;
        }

        authFetch('/api/auth/user')
            .then(async (response) => {
                if (response.ok) {
                    const data = await response.json() as User;
                    setUser(data);
                } else {
                    removeAuthToken();
                }
            })
            .catch(() => removeAuthToken());
    }, []);

    function login() {
        window.location.href = '/api/auth/login';
    }

    function logout() {
        removeAuthToken();
        setUser(null);
    }

    const isAdmin = user?.roles.includes('Admin') ?? false;

    return (
        <AuthContext.Provider value={{ user, login, logout, isAdmin }}>
            {children}
        </AuthContext.Provider>
    );
}
