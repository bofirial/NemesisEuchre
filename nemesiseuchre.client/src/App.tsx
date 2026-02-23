import { LoginButton } from './components/LoginButton';
import { useAuth } from './auth/useAuth';

function App() {
    const { user } = useAuth();

    return (
        <div>
            <header>
                <h1>NemesisEuchre</h1>
                <LoginButton />
            </header>
            <main>
                {user ? (
                    <p>Welcome, {user.name}!</p>
                ) : (
                    <p>Sign in with GitHub to get started.</p>
                )}
            </main>
        </div>
    );
}

export default App;
