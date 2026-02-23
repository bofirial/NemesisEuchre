import { useAuth } from '../auth/useAuth';

export function LoginButton() {
    const { user, login, logout } = useAuth();

    if (!user) {
        return <button onClick={login}>Login with GitHub</button>;
    }

    return (
        <span>
            {user.name}
            <button onClick={logout}>Logout</button>
        </span>
    );
}
