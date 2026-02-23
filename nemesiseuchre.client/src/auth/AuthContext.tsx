import { createContext } from 'react';

export interface User {
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


