import { useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';

import { useAuth } from '@/auth/useAuth';
import { BotUploadForm } from '@/components/BotUploadForm';

export function UploadBotPage() {
    const { isAdmin } = useAuth();
    const navigate = useNavigate();

    useEffect(() => {
        if (!isAdmin) navigate('/');
    }, [isAdmin, navigate]);

    if (!isAdmin) return null;

    return (
        <div className="flex flex-col items-center gap-6">
            <div className="w-full max-w-lg">
                <Link to="/admin" className="text-sm text-muted-foreground hover:text-foreground">← Admin</Link>
            </div>
            <h1 className="text-4xl font-bold tracking-tight">Create Bot</h1>
            <BotUploadForm mode="create" onSuccess={() => navigate('/admin')} />
        </div>
    );
}
