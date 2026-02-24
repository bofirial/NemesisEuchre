import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';

import { useAuth } from '@/auth/useAuth';
import { Button } from '@/components/ui/button';

export function AdminPage() {
    const { isAdmin } = useAuth();
    const navigate = useNavigate();

    const [bots, setBots] = useState<string[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        if (!isAdmin) {
            navigate('/');
        }
    }, [isAdmin, navigate]);

    useEffect(() => {
        if (!isAdmin) return;

        async function fetchBots() {
            const token = sessionStorage.getItem('auth_token');
            try {
                const response = await fetch('/api/admin/bots', {
                    headers: token ? { Authorization: `Bearer ${token}` } : {},
                });
                if (response.ok) {
                    const data = await response.json() as { bots: string[] };
                    setBots(data.bots);
                } else if (response.status === 503) {
                    setError('Azure Storage is not configured on the server.');
                } else {
                    setError(`Failed to load bots (${response.status}).`);
                }
            } catch {
                setError('Network error. Please try again.');
            } finally {
                setLoading(false);
            }
        }

        void fetchBots();
    }, [isAdmin]);

    if (!isAdmin) return null;

    return (
        <div className="flex flex-col items-center gap-6">
            <h1 className="text-4xl font-bold tracking-tight">Admin</h1>

            <div className="w-full max-w-lg flex flex-col gap-4">
                <h2 className="text-2xl font-semibold">Bots</h2>

                {loading && <p className="text-sm text-muted-foreground">Loading bots...</p>}
                {error && <p className="text-sm text-destructive">{error}</p>}
                {!loading && !error && bots.length === 0 && (
                    <p className="text-sm text-muted-foreground">No bots uploaded yet.</p>
                )}
                {!loading && !error && bots.length > 0 && (
                    <ul className="flex flex-col gap-2">
                        {bots.map(bot => (
                            <li key={bot} className="flex items-center justify-between gap-4 rounded border border-border px-4 py-2">
                                <span className="text-sm font-medium">{bot}</span>
                                <Button
                                    variant="outline"
                                    size="sm"
                                    onClick={() => navigate(`/admin/bots/${encodeURIComponent(bot)}`)}
                                >
                                    Edit
                                </Button>
                            </li>
                        ))}
                    </ul>
                )}

                <Button onClick={() => navigate('/admin/upload-bot')}>Upload New Bot</Button>
            </div>
        </div>
    );
}
