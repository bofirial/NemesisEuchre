const TOKEN_KEY = 'auth_token';

export function getAuthToken(): string | null {
    return sessionStorage.getItem(TOKEN_KEY);
}

export function setAuthToken(token: string): void {
    sessionStorage.setItem(TOKEN_KEY, token);
}

export function removeAuthToken(): void {
    sessionStorage.removeItem(TOKEN_KEY);
}

export function authFetch(url: string, init?: RequestInit): Promise<Response> {
    const token = getAuthToken();
    return fetch(url, {
        ...init,
        headers: {
            ...(token ? { Authorization: `Bearer ${token}` } : {}),
            ...init?.headers,
        },
    });
}
