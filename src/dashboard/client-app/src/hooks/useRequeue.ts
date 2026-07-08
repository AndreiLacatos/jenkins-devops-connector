import { useState } from "react";

export function useRequeue() {
    const [loading, setLoading] = useState(false);

    const submit = async (name: string, build: number, commit: string) => {
        if (loading) {
            return false;
        }

        setLoading(true);
        try {
            const res = await fetch(
                `/api/jobs/requeue`,
                {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify({
                        name,
                        build,
                        commit,
                    }),
                }
            );

            if (!res.ok) {
                throw new Error(`HTTP ${res.status}`);
            }

            return true;
        } catch {
            return false;
        } finally {
            setLoading(false);
        }
    };

    return { submit, loading };
}