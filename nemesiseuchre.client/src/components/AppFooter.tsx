import { useEffect, useState } from 'react';

export function AppFooter() {
    const [version, setVersion] = useState<string | null>(null);

    useEffect(() => {
        fetch('/api/version')
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
