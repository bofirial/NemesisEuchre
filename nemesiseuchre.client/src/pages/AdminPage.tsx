import { useEffect, useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';

import { useAuth } from '@/auth/useAuth';
import { Button } from '@/components/ui/button';

interface FileSpec {
    field: string;
    label: string;
    requiredSuffix: string;
}

const FILE_SPECS: FileSpec[] = [
    { field: 'callTrumpZip', label: 'Model (.zip)', requiredSuffix: '_calltrump.zip' },
    { field: 'callTrumpJson', label: 'Metadata (.json)', requiredSuffix: '_calltrump.json' },
    { field: 'discardCardZip', label: 'Model (.zip)', requiredSuffix: '_discardcard.zip' },
    { field: 'discardCardJson', label: 'Metadata (.json)', requiredSuffix: '_discardcard.json' },
    { field: 'playCardZip', label: 'Model (.zip)', requiredSuffix: '_playcard.zip' },
    { field: 'playCardJson', label: 'Metadata (.json)', requiredSuffix: '_playcard.json' },
];

const GROUPS = [
    { label: 'Call Trump', specs: FILE_SPECS.slice(0, 2) },
    { label: 'Discard Card', specs: FILE_SPECS.slice(2, 4) },
    { label: 'Play Card', specs: FILE_SPECS.slice(4, 6) },
];

const MODEL_NAME_REGEX = /^[a-zA-Z0-9_-]{1,50}$/;

function validateModelName(name: string): string | null {
    if (!name) return 'Model name is required.';
    if (!MODEL_NAME_REGEX.test(name)) return 'Must be 1–50 characters: letters, digits, hyphens, underscores.';
    return null;
}

function assignFiles(fileList: FileList): { assigned: Record<string, File | null>; unrecognized: string[] } {
    const assigned: Record<string, File | null> = Object.fromEntries(FILE_SPECS.map(s => [s.field, null]));
    const unrecognized: string[] = [];
    for (const file of Array.from(fileList)) {
        const spec = FILE_SPECS.find(s => file.name.toLowerCase().endsWith(s.requiredSuffix));
        if (spec) {
            assigned[spec.field] = file;
        } else {
            unrecognized.push(file.name);
        }
    }
    return { assigned, unrecognized };
}

export function AdminPage() {
    const { isAdmin } = useAuth();
    const navigate = useNavigate();

    const [modelName, setModelName] = useState('');
    const [files, setFiles] = useState<Record<string, File | null>>(() =>
        Object.fromEntries(FILE_SPECS.map(s => [s.field, null]))
    );
    const [unrecognized, setUnrecognized] = useState<string[]>([]);
    const [touched, setTouched] = useState<Record<string, boolean>>({});
    const [uploading, setUploading] = useState(false);
    const [status, setStatus] = useState<{ type: 'success' | 'error'; message: string } | null>(null);

    const fileInputRef = useRef<HTMLInputElement | null>(null);

    useEffect(() => {
        if (!isAdmin) navigate('/');
    }, [isAdmin, navigate]);

    if (!isAdmin) return null;

    const nameError = validateModelName(modelName);
    const missingFiles = FILE_SPECS.filter(s => !files[s.field]);

    const allErrors: string[] = [
        ...(nameError ? [`Model name: ${nameError}`] : []),
        ...(missingFiles.length > 0 ? [`Missing files: ${missingFiles.map(s => s.requiredSuffix).join(', ')}`] : []),
    ];
    const isValid = allErrors.length === 0;
    const anyTouched = Object.values(touched).some(Boolean);

    function handleFilesChange(e: React.ChangeEvent<HTMLInputElement>) {
        if (!e.target.files || e.target.files.length === 0) return;
        const { assigned, unrecognized: unrecog } = assignFiles(e.target.files);
        setFiles(prev => ({ ...prev, ...assigned }));
        setUnrecognized(unrecog);
        setTouched(prev => ({ ...prev, files: true }));
        setStatus(null);
    }

    async function handleSubmit(e: React.FormEvent) {
        e.preventDefault();
        if (!isValid || uploading) return;

        setUploading(true);
        setStatus(null);

        const formData = new FormData();
        formData.append('modelName', modelName);
        for (const spec of FILE_SPECS) {
            formData.append(spec.field, files[spec.field]!);
        }

        const token = sessionStorage.getItem('auth_token');
        try {
            const response = await fetch('/api/admin/models', {
                method: 'POST',
                headers: token ? { Authorization: `Bearer ${token}` } : {},
                body: formData,
            });

            if (response.ok) {
                setStatus({ type: 'success', message: 'Upload successful!' });
                setModelName('');
                setFiles(Object.fromEntries(FILE_SPECS.map(s => [s.field, null])));
                setUnrecognized([]);
                setTouched({});
                if (fileInputRef.current) fileInputRef.current.value = '';
            } else if (response.status === 503) {
                setStatus({ type: 'error', message: 'Azure Storage is not configured on the server.' });
            } else if (response.status === 401 || response.status === 403) {
                setStatus({ type: 'error', message: 'Unauthorized. Admin role required.' });
            } else {
                const body = await response.json().catch(() => null) as { errors?: string[] } | null;
                const msg = body?.errors?.join(' ') ?? `Upload failed (${response.status}).`;
                setStatus({ type: 'error', message: msg });
            }
        } catch {
            setStatus({ type: 'error', message: 'Network error. Please try again.' });
        } finally {
            setUploading(false);
        }
    }

    return (
        <div className="flex flex-col items-center gap-6">
            <h1 className="text-4xl font-bold tracking-tight">Admin</h1>

            <form onSubmit={handleSubmit} className="w-full max-w-lg flex flex-col gap-6">
                <h2 className="text-2xl font-semibold">Upload Model</h2>

                <div className="flex flex-col gap-1">
                    <label className="text-sm font-medium" htmlFor="modelName">Model Name</label>
                    <input
                        id="modelName"
                        type="text"
                        value={modelName}
                        onChange={e => { setModelName(e.target.value); setTouched(prev => ({ ...prev, modelName: true })); setStatus(null); }}
                        placeholder="e.g. my-model-v1"
                        className="border rounded px-3 py-2 text-sm bg-background"
                    />
                    {touched['modelName'] && nameError && (
                        <p className="text-xs text-destructive">{nameError}</p>
                    )}
                </div>

                <div className="flex flex-col gap-3">
                    <div className="flex items-center gap-3">
                        <label
                            htmlFor="modelFiles"
                            className="cursor-pointer inline-flex items-center px-3 py-1.5 rounded border border-input bg-background text-sm hover:bg-accent hover:text-accent-foreground transition-colors select-none shrink-0"
                        >
                            Choose files
                        </label>
                        <span className="text-xs text-muted-foreground">Select all 6 model files at once (.zip + .json for each decision type)</span>
                        <input
                            id="modelFiles"
                            type="file"
                            accept=".zip,.json"
                            multiple
                            ref={fileInputRef}
                            onChange={handleFilesChange}
                            className="sr-only"
                        />
                    </div>

                    {unrecognized.length > 0 && (
                        <p className="text-xs text-destructive">
                            Ignored (unrecognized filenames): {unrecognized.join(', ')}
                        </p>
                    )}

                    <div className="flex flex-col gap-4 rounded border border-border p-4">
                        {GROUPS.map(group => (
                            <div key={group.label} className="flex flex-col gap-1.5">
                                <p className="text-sm font-semibold">{group.label}</p>
                                {group.specs.map(spec => {
                                    const file = files[spec.field];
                                    return (
                                        <div key={spec.field} className="flex items-center gap-2 text-sm">
                                            <span className={file ? 'text-green-600' : 'text-muted-foreground'}>
                                                {file ? '✓' : '○'}
                                            </span>
                                            <span className="text-muted-foreground w-28 shrink-0">{spec.label}</span>
                                            <span className={`flex-1 min-w-0 truncate ${file ? 'text-foreground' : 'text-muted-foreground italic'}`}>
                                                {file ? file.name : `*${spec.requiredSuffix}`}
                                            </span>
                                        </div>
                                    );
                                })}
                            </div>
                        ))}
                    </div>
                </div>

                {anyTouched && !isValid && (
                    <div className="rounded border border-destructive/40 bg-destructive/5 px-4 py-3 flex flex-col gap-1">
                        <p className="text-sm font-medium text-destructive">Please fix the following:</p>
                        <ul className="list-disc list-inside text-xs text-destructive space-y-0.5">
                            {allErrors.map(err => <li key={err}>{err}</li>)}
                        </ul>
                    </div>
                )}

                <Button type="submit" disabled={!isValid || uploading}>
                    {uploading ? 'Uploading...' : 'Upload Model'}
                </Button>

                {status && (
                    <p className={`text-sm ${status.type === 'success' ? 'text-green-600' : 'text-destructive'}`}>
                        {status.message}
                    </p>
                )}
            </form>
        </div>
    );
}
