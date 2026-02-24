import { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '@/auth/useAuth';

export function AdminPage() {
    const { isAdmin } = useAuth();
    const navigate = useNavigate();

    useEffect(() => {
        if (!isAdmin) {
            navigate('/');
        }
    }, [isAdmin, navigate]);

    if (!isAdmin) return null;

    return (
        <div className="flex flex-col items-center gap-6">
            <h1 className="text-4xl font-bold tracking-tight">Admin</h1>
            <p className="text-muted-foreground">Admin controls will appear here.</p>
        </div>
    );
}
