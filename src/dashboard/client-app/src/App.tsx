import Dashboard from "./Dashboard";
import { useHealth } from "./hooks/useHealth";

export default function App() {
  const { health, loading, error } = useHealth();

  const isHealthy = !loading && !error && health?.healthy;

  return (
    <div className="min-h-screen w-full bg-white flex items-start justify-center p-6">
      <div className="w-full px-0 lg:px-[15%]">
        {loading && (
          <div className="flex items-center justify-center py-10">
            <div className="h-10 w-10 border-4 border-slate-300 border-t-cyan-500 rounded-full animate-spin" />
          </div>
        )}

        {!loading && error && (
          <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded">
            <strong className="font-semibold">Error:</strong> {error}
          </div>
        )}

        {!loading && !error && health && !health.healthy && (
          <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded">
            <div className="font-semibold mb-2">System unhealthy</div>

            {health.failures?.length ? (
              <ul className="list-disc pl-5">
                {health.failures.map((f, i) => (
                  <li key={i}>{f}</li>
                ))}
              </ul>
            ) : (
              <div>No failure details provided</div>
            )}
          </div>
        )}

        {isHealthy && <Dashboard />}
      </div>
    </div>
  );
}