import { useEffect, useState } from "react";

export type HealthModel = {
  healthy: boolean;
  failures?: string[] | null;
};

export function useHealth() {
  const [data, setData] = useState<HealthModel | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;
    let timeoutId: ReturnType<typeof setTimeout>;

    async function fetchHealth() {
      try {
        setLoading(true);

        const res = await fetch("/api/daemon-health");

        if (!res.ok) {
          throw new Error(`HTTP ${res.status}`);
        }

        const json: HealthModel = await res.json();

        if (cancelled) return;

        setData(json);
        setError(null);
        setLoading(false);
      } catch (e: any) {
        if (cancelled) return;

        setError(e.message ?? "Unknown error");
        setData(null);
        setLoading(false);

        // retry in 5 seconds
        timeoutId = setTimeout(() => {
          fetchHealth();
        }, 5000);
      }
    }

    fetchHealth();

    return () => {
      cancelled = true;
      clearTimeout(timeoutId);
    };
  }, []);

  return { health: data, loading, error };
}