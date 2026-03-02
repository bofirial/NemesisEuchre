import { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

import { useAuth } from './useAuth';

export function useAdminGuard(): boolean {
    const { isAdmin } = useAuth();
    const navigate = useNavigate();

    useEffect(() => {
        if (!isAdmin) navigate('/');
    }, [isAdmin, navigate]);

    return isAdmin;
}
