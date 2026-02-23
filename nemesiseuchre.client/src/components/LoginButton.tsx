import { Github } from 'lucide-react';

import { useAuth } from '@/auth/useAuth';
import { Button } from '@/components/ui/button';

export function NavLoginButton() {
    const { user, login, logout } = useAuth();

    if (!user) {
        return (
            <Button variant="ghost" size="sm" onClick={login}>
                <Github className="mr-2 h-4 w-4" />
                Login with GitHub
            </Button>
        );
    }

    return (
        <Button variant="ghost" size="sm" onClick={logout}>
            Logout
        </Button>
    );
}

export function LoginCta() {
    const { login } = useAuth();

    return (
        <Button size="lg" onClick={login}>
            <Github className="mr-2 h-5 w-5" />
            Login with GitHub
        </Button>
    );
}
