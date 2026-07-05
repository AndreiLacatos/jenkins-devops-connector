import { useEffect, useState } from "react";

export type JobApiModel = {
    name: string;
    build: number;
    commit: string;
    jobEvent: string;
    buildUrl?: string | null;
    syncStatus: string;
    registeredAt: string;
    enqueuedAt?: string | null;
    finishedAt?: string | null;
};

export type RepositoryJobsApiModel = {
    repository: string;
    branchJobs: Record<string, JobApiModel>;
};

export type JobListResponseApiModel = {
    jobs: RepositoryJobsApiModel[];
};

export function useRecentJobs() {
    const [data, setData] = useState<JobListResponseApiModel | null>(null);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        let cancelled = false;
        let intervalId: ReturnType<typeof setInterval>;

        async function fetchJobs(refresh: boolean) {
            try {
                if (!refresh) {
                    setLoading(true);
                }

                const res = await fetch("api/recent-jobs");

                if (!res.ok) {
                    throw new Error(`HTTP ${res.status}`);
                }

                const json: JobListResponseApiModel = await res.json();

                if (cancelled) return;

                setData(json);
                setError(null);
                setLoading(false);
            } catch (e: any) {
                if (cancelled) return;

                setError(e.message ?? "Unknown error");
                setLoading(false);
            }
        }

        // initial load
        fetchJobs(false);

        // polling every 10s after success
        intervalId = setInterval(() => {
            fetchJobs(true);
        }, 10000);

        return () => {
            cancelled = true;
            clearInterval(intervalId);
        };
    }, []);

    return { data, loading, error };
}