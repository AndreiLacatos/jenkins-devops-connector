import { useState } from "react";

export function useRequeue() {
    const [loading, setLoading] = useState<boolean>(false);
    const [error, setError] = useState<string | null>(null);

    const submit = async (name: string, build: number, commit: string) => {
        if (loading) {
            return;
        }

        setLoading(true);
        try {
            const res = await fetch(
                `/pi/jobs/${encodeURIComponent(name)}/${build}/${encodeURIComponent(commit)}/requeue`,
                { method: 'POST' });

            if (!res.ok) {
                throw new Error(`HTTP ${res.status}`);
            }

            setError(null);
            setLoading(false);
        }
        catch (e: any) {
            setError(e.message ?? "Unknown error");
            setLoading(false);
        }
    }

    return { submit, loading, error };
}