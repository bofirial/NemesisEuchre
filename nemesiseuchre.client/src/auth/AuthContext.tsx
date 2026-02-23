import { createContext, useEffect, useState } from 'react';
import type { ReactNode } from 'react';

interface User {
    name: string;
    login: string;
    email: string | null;
    roles: string[];
}

interface AuthContextValue {
    user: User | null;
    login: () => void;
    logout: () => void;
    isAdmin: boolean;
}

export const AuthContext = createContext<AuthContextValue>({
    user: null,
    login: () => {},
    logout: () => {},
    isAdmin: false,
});

export function AuthProvider({ children }: { children: ReactNode }) {
    const [user, setUser] = useState<User | null>(null);

    useEffect(() => {
        const hash = window.location.hash;
        const params = new URLSearchParams(hash.slice(1));
        const token = params.get('token');

        if (token) {
            sessionStorage.setItem('auth_token', token);
            window.history.replaceState(null, '', window.location.pathname + window.location.search);
        }

        const storedToken = sessionStorage.getItem('auth_token');
        if (!storedToken) {
            return;
        }

        fetch('/api/auth/user', { headers: { Authorization: `Bearer ${storedToken}` } })
            .then(async (response) => {
                if (response.ok) {
                    const data = await response.json() as User;
                    setUser(data);
                } else {
                    sessionStorage.removeItem('auth_token');
                }
            })
            .catch(() => sessionStorage.removeItem('auth_token'));
    }, []);

    function login() {
        window.location.href = '/api/auth/login';
    }

    function logout() {
        sessionStorage.removeItem('auth_token');
        setUser(null);
    }

    const isAdmin = user?.roles.includes('Admin') ?? false;

    return (
        <AuthContext.Provider value={{ user, login, logout, isAdmin }}>
            {children}
        </AuthContext.Provider>
    );
}
