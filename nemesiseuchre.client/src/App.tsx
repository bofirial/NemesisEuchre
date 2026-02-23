import { useAuth } from '@/auth/useAuth';
import { LoginCta, NavLoginButton } from '@/components/LoginButton';

function App() {
    const { user } = useAuth();

    return (
        <div className="min-h-svh bg-background text-foreground">
            <header className="sticky top-0 z-50 border-b border-border bg-background/95 backdrop-blur-sm">
                <div className="mx-auto flex h-14 max-w-5xl items-center justify-between px-4">
                    <div className="flex items-center gap-2">
                        <img src="/nemesiseuchreLogo.svg" alt="NemesisEuchre" className="h-8 w-8" />
                        <span className="font-semibold tracking-tight">
                            <span className="text-brand-blue">Nemesis</span>
                            <span className="text-brand-red">Euchre</span>
                        </span>
                    </div>
                    <NavLoginButton />
                </div>
            </header>

            <main className="mx-auto max-w-5xl px-4 py-24 text-center">
                {user ? (
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
                        <p className="text-muted-foreground">Game lobby coming soon.</p>
                    </div>
                ) : (
                    <div className="flex flex-col items-center gap-6">
                        <h1 className="text-4xl font-bold tracking-tight">
                            Welcome to{' '}
                            <span className="text-brand-blue">Nemesis</span>
                            <span className="text-brand-red">Euchre</span>
                        </h1>
                        <p className="text-lg text-muted-foreground">AI-powered Euchre training engine</p>
                        <LoginCta />
                    </div>
                )}
            </main>
        </div>
    );
}

export default App;
