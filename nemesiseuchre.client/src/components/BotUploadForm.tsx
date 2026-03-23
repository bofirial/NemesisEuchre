import { useEffect, useRef, useState } from 'react';

import { authFetch } from '@/api/fetchUtils';
import { Button } from '@/components/ui/button';

interface FileSpec {
    field: string;
    label: string;
    requiredSuffix: string;
}

const FILE_SPECS: FileSpec[] = [
    { field: 'callTrumpZip', label: 'Model (.zip)', requiredSuffix: '_calltrump.zip' },
    { field: 'callTrumpJson', label: 'Metadata (.json)', requiredSuffix: '_calltrump.json' },
    { field: 'advancedCallTrumpZip', label: 'Model (.zip)', requiredSuffix: '_advancedcalltrump.zip' },
    { field: 'advancedCallTrumpJson', label: 'Metadata (.json)', requiredSuffix: '_advancedcalltrump.json' },
    { field: 'discardCardZip', label: 'Model (.zip)', requiredSuffix: '_discardcard.zip' },
    { field: 'discardCardJson', label: 'Metadata (.json)', requiredSuffix: '_discardcard.json' },
    { field: 'advancedDiscardCardZip', label: 'Model (.zip)', requiredSuffix: '_advanceddiscardcard.zip' },
    { field: 'advancedDiscardCardJson', label: 'Metadata (.json)', requiredSuffix: '_advanceddiscardcard.json' },
    { field: 'playCardZip', label: 'Model (.zip)', requiredSuffix: '_playcard.zip' },
    { field: 'playCardJson', label: 'Metadata (.json)', requiredSuffix: '_playcard.json' },
    { field: 'simplePlayCardZip', label: 'Model (.zip)', requiredSuffix: '_simpleplaycard.zip' },
    { field: 'simplePlayCardJson', label: 'Metadata (.json)', requiredSuffix: '_simpleplaycard.json' },
    { field: 'advancedPlayCardZip', label: 'Model (.zip)', requiredSuffix: '_advancedplaycard.zip' },
    { field: 'advancedPlayCardJson', label: 'Metadata (.json)', requiredSuffix: '_advancedplaycard.json' },
];

interface VariantSpec {
    variantLabel: string;
    prefix: string;
    specs: FileSpec[];
}

interface CategorySpec {
    label: string;
    variants: VariantSpec[];
}

function specsForPrefix(prefix: string): FileSpec[] {
    return FILE_SPECS.filter(s => s.field.startsWith(prefix));
}

const CATEGORIES: CategorySpec[] = [
    {
        label: 'Call Trump',
        variants: [
            { variantLabel: 'Call Trump', prefix: 'callTrump', specs: specsForPrefix('callTrump') },
            { variantLabel: 'Advanced Call Trump', prefix: 'advancedCallTrump', specs: specsForPrefix('advancedCallTrump') },
        ],
    },
    {
        label: 'Discard Card',
        variants: [
            { variantLabel: 'Discard Card', prefix: 'discardCard', specs: specsForPrefix('discardCard') },
            { variantLabel: 'Advanced Discard Card', prefix: 'advancedDiscardCard', specs: specsForPrefix('advancedDiscardCard') },
        ],
    },
    {
        label: 'Play Card',
        variants: [
            { variantLabel: 'Play Card', prefix: 'playCard', specs: specsForPrefix('playCard') },
            { variantLabel: 'Simple Play Card', prefix: 'simplePlayCard', specs: specsForPrefix('simplePlayCard') },
            { variantLabel: 'Advanced Play Card', prefix: 'advancedPlayCard', specs: specsForPrefix('advancedPlayCard') },
        ],
    },
];

const BOT_NAME_REGEX = /^[a-zA-Z0-9 _-]{1,50}$/;

function validateBotName(name: string): string | null {
    if (!name) return 'Bot name is required.';
    if (!BOT_NAME_REGEX.test(name)) return 'Must be 1–50 characters: letters, digits, spaces, hyphens, underscores.';
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

function variantHasFiles(variant: VariantSpec, files: Record<string, File | null>): boolean {
    return variant.specs.some(s => files[s.field] !== null);
}

function validateCategories(files: Record<string, File | null>): string[] {
    const errors: string[] = [];
    for (const category of CATEGORIES) {
        const filledVariants = category.variants.filter(v => variantHasFiles(v, files));
        if (filledVariants.length === 0) {
            const names = category.variants.map(v => v.variantLabel).join(' or ');
            errors.push(`${category.label}: provide one variant (${names}).`);
        } else if (filledVariants.length > 1) {
            errors.push(`${category.label}: provide only one variant, not multiple.`);
        } else {
            const variant = filledVariants[0];
            for (const spec of variant.specs) {
                if (!files[spec.field]) {
                    errors.push(`${variant.variantLabel}: ${spec.label} is required.`);
                }
            }
        }
    }
    return errors;
}

export interface BotUploadFormProps {
    mode: 'create' | 'edit';
    initialBotName?: string;
    onSuccess: (_botName: string) => void;
}

export function BotUploadForm({ mode, initialBotName, onSuccess }: BotUploadFormProps) {
    const [botName, setBotName] = useState(initialBotName ?? '');
    const [files, setFiles] = useState<Record<string, File | null>>(() =>
        Object.fromEntries(FILE_SPECS.map(s => [s.field, null]))
    );
    const [existingFiles, setExistingFiles] = useState<Record<string, string | null>>(() =>
        Object.fromEntries(FILE_SPECS.map(s => [s.field, null]))
    );
    const [unrecognized, setUnrecognized] = useState<string[]>([]);
    const [touched, setTouched] = useState<Record<string, boolean>>({});
    const [uploading, setUploading] = useState(false);
    const [status, setStatus] = useState<{ type: 'success' | 'error'; message: string } | null>(null);

    const fileInputRef = useRef<HTMLInputElement | null>(null);

    useEffect(() => {
        if (mode !== 'edit' || !initialBotName) return;

        async function fetchExistingFiles() {
            const response = await authFetch(`/api/admin/bots/${encodeURIComponent(initialBotName!)}`);
            if (!response.ok) return;
            const data = await response.json() as { files: string[] };
            const mapped: Record<string, string | null> = Object.fromEntries(FILE_SPECS.map(s => [s.field, null]));
            for (const fileName of data.files) {
                const spec = FILE_SPECS.find(s => fileName.toLowerCase().endsWith(s.requiredSuffix));
                if (spec) mapped[spec.field] = fileName;
            }
            setExistingFiles(mapped);
        }

        void fetchExistingFiles();
    }, [mode, initialBotName]);

    const nameError = validateBotName(botName.trim());
    const categoryErrors = mode === 'create' ? validateCategories(files) : [];

    const allErrors: string[] = [
        ...(nameError ? [`Bot name: ${nameError}`] : []),
        ...categoryErrors,
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
        let url: string;
        let method: string;

        if (mode === 'create') {
            url = '/api/admin/bots';
            method = 'POST';
            formData.append('botName', botName.trim());
            for (const spec of FILE_SPECS) {
                if (files[spec.field]) {
                    formData.append(spec.field, files[spec.field]!);
                }
            }
        } else {
            url = `/api/admin/bots/${encodeURIComponent(initialBotName!)}`;
            method = 'PUT';
            if (botName.trim() !== initialBotName) {
                formData.append('newBotName', botName.trim());
            }
            for (const spec of FILE_SPECS) {
                if (files[spec.field]) {
                    formData.append(spec.field, files[spec.field]!);
                }
            }
        }

        try {
            const response = await authFetch(url, { method, body: formData });

            if (response.ok) {
                onSuccess(botName.trim());
            } else if (response.status === 503) {
                setStatus({ type: 'error', message: 'Azure Storage is not configured on the server.' });
            } else if (response.status === 401 || response.status === 403) {
                setStatus({ type: 'error', message: 'Unauthorized. Admin role required.' });
            } else {
                const body = await response.json().catch(() => null) as { errors?: string[] } | null;
                const msg = body?.errors?.join(' ') ?? `Request failed (${response.status}).`;
                setStatus({ type: 'error', message: msg });
            }
        } catch {
            setStatus({ type: 'error', message: 'Network error. Please try again.' });
        } finally {
            setUploading(false);
        }
    }

    return (
        <form onSubmit={handleSubmit} className="w-full max-w-lg flex flex-col gap-6">
            <div className="flex flex-col gap-1">
                <label className="text-sm font-medium" htmlFor="botName">Bot Name</label>
                <input
                    id="botName"
                    type="text"
                    value={botName}
                    onChange={e => { setBotName(e.target.value); setTouched(prev => ({ ...prev, botName: true })); setStatus(null); }}
                    placeholder="e.g. My Bot V1"
                    className="border rounded px-3 py-2 text-sm bg-background"
                />
                {touched['botName'] && nameError && (
                    <p className="text-xs text-destructive">{nameError}</p>
                )}
            </div>

            <div className="flex flex-col gap-3">
                <div className="flex items-center gap-3">
                    <label
                        htmlFor="botFiles"
                        className="cursor-pointer inline-flex items-center px-3 py-1.5 rounded border border-input bg-background text-sm hover:bg-accent hover:text-accent-foreground transition-colors select-none shrink-0"
                    >
                        Choose files
                    </label>
                    <span className="text-xs text-muted-foreground">
                        {mode === 'create'
                            ? 'Select model files (.zip + .json for each decision type)'
                            : 'Optionally replace model files (.zip + .json for each decision type)'}
                    </span>
                    <input
                        id="botFiles"
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
                    {CATEGORIES.map(category => (
                        <div key={category.label} className="flex flex-col gap-2">
                            <p className="text-sm font-semibold">{category.label}</p>
                            {category.variants.map(variant => {
                                const hasAny = variantHasFiles(variant, files) ||
                                    variant.specs.some(s => existingFiles[s.field]);
                                return (
                                    <div key={variant.prefix} className={`flex flex-col gap-1 pl-3 ${!hasAny ? 'opacity-50' : ''}`}>
                                        <p className="text-xs font-medium text-muted-foreground">{variant.variantLabel}</p>
                                        {variant.specs.map(spec => {
                                            const newFile = files[spec.field];
                                            const existingFile = existingFiles[spec.field];
                                            return (
                                                <div key={spec.field} className="flex items-center gap-2 text-sm">
                                                    <span className={newFile ? 'text-green-600' : 'text-muted-foreground'}>
                                                        {newFile ? '✓' : '○'}
                                                    </span>
                                                    <span className="text-muted-foreground w-28 shrink-0">{spec.label}</span>
                                                    <span className={`flex-1 min-w-0 truncate ${newFile ? 'text-foreground' : existingFile ? 'text-muted-foreground' : 'text-muted-foreground italic'}`}>
                                                        {newFile ? newFile.name : existingFile ?? `*${spec.requiredSuffix}`}
                                                    </span>
                                                </div>
                                            );
                                        })}
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
                {uploading ? 'Saving...' : mode === 'create' ? 'Upload Bot' : 'Save Changes'}
            </Button>

            {status && (
                <p className={`text-sm ${status.type === 'success' ? 'text-green-600' : 'text-destructive'}`}>
                    {status.message}
                </p>
            )}
        </form>
    );
}
