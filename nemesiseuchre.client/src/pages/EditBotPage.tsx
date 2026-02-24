import { useEffect } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';

import { useAuth } from '@/auth/useAuth';
import { BotUploadForm } from '@/components/BotUploadForm';
import { Button } from '@/components/ui/button';

export function EditBotPage() {
    const { botName } = useParams<{ botName: string }>();
    const { isAdmin } = useAuth();
    const navigate = useNavigate();

    useEffect(() => {
        if (!isAdmin) navigate('/');
    }, [isAdmin, navigate]);

    if (!isAdmin || !botName) return null;

    function handleSaveSuccess(resultBotName: string) {
        if (resultBotName !== botName) {
            navigate(`/admin/bots/${encodeURIComponent(resultBotName)}`);
        } else {
            navigate('/admin');
        }
    }

    async function handleDelete() {
        if (!confirm(`Delete bot "${botName}"? This cannot be undone.`)) return;

        const token = sessionStorage.getItem('auth_token');
        try {
            const response = await fetch(`/api/admin/bots/${encodeURIComponent(botName!)}`, {
                method: 'DELETE',
                headers: token ? { Authorization: `Bearer ${token}` } : {},
            });
            if (response.ok) {
                navigate('/admin');
            } else if (response.status === 503) {
                alert('Azure Storage is not configured on the server.');
            } else {
                alert(`Delete failed (${response.status}).`);
            }
        } catch {
            alert('Network error. Please try again.');
        }
    }

    return (
        <div className="flex flex-col items-center gap-6">
            <div className="w-full max-w-lg">
                <Link to="/admin" className="text-sm text-muted-foreground hover:text-foreground">← Admin</Link>
            </div>
            <h1 className="text-4xl font-bold tracking-tight">Edit Bot</h1>
            <BotUploadForm mode="edit" initialBotName={botName} onSuccess={handleSaveSuccess} />
            <div className="w-full max-w-lg border-t border-border pt-6">
                <Button variant="destructive" onClick={handleDelete}>
                    Delete Bot
                </Button>
            </div>
        </div>
    );
}
