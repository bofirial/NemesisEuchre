import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '@/auth/useAuth';
import { LoginCta } from '@/components/LoginButton';
import { Button } from '@/components/ui/button';

export function HomePage() {
    const { user, isAdmin } = useAuth();
    const navigate = useNavigate();
    const [sessionName, setSessionName] = useState('');

    if (user) {
        return (
            <div className="flex flex-col items-center gap-6">
                <img
                    src={`https://github.com/${user.login}.png`}
                    alt={user.name}
                    width={80}
                    height={80}
                    className="rounded-full border border-border"
                />
                <h1 className="text-4xl font-bold tracking-tight">
                    Welcome back, {user.name}!
                </h1>
                <div className="flex items-center gap-2">
                    <input
                        type="text"
                        placeholder="Session name"
                        value={sessionName}
                        onChange={(e) => setSessionName(e.target.value)}
                        onKeyDown={(e) => {
                            if (e.key === 'Enter' && sessionName.trim()) {
                                navigate(`/game/${encodeURIComponent(sessionName.trim())}`);
                            }
                        }}
                        className="h-9 rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-xs placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring transition-colors"
                    />
                    <Button
                        disabled={!sessionName.trim()}
                        onClick={() => navigate(`/game/${encodeURIComponent(sessionName.trim())}`)}
                    >
                        Join Game
                    </Button>
                </div>
                {isAdmin && (
                    <Button variant="outline" onClick={() => navigate('/admin')}>
                        Admin
                    </Button>
                )}
            </div>
        );
    }

    return (
        <div className="flex flex-col items-center gap-6">
            <h1 className="text-4xl font-bold tracking-tight">
                Welcome to{' '}
                <span className="text-brand-blue">Nemesis</span>
                <span className="text-brand-red">Euchre</span>
            </h1>
            <p className="text-lg text-muted-foreground">AI-powered Euchre training engine</p>
            <LoginCta />
        </div>
    );
}
