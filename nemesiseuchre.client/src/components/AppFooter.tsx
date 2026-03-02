import { useEffect, useState } from 'react';

import { authFetch } from '@/api/fetchUtils';

export function AppFooter() {
    const [version, setVersion] = useState<string | null>(null);

    useEffect(() => {
        authFetch('/api/version')
            .then(r => r.json())
            .then(data => setVersion(data.version))
            .catch(() => { /* silently ignore */ });
    }, []);

    return (
        <footer className="border-t border-border py-4 text-center text-xs text-muted-foreground">
            {version ? `v${version}` : null}
        </footer>
    );
}
