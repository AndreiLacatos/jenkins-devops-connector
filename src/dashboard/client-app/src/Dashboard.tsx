import { useMemo, useState } from "react";
import { useRecentJobs } from "./hooks/useRecentJobs";
import DashboardJobItem from "./DashboardJobItem";

export default function JobsDashboard() {
  const { data, loading, error } = useRecentJobs();
  const [selectedRepo, setSelectedRepo] = useState<string | null>(null);

  const repos = data?.jobs ?? [];

  const activeRepo = useMemo(() => {
    if (!selectedRepo) return repos[0];
    return repos.find(r => r.repositoryName === selectedRepo);
  }, [selectedRepo, repos]);

  if (loading) {
    return (
      <div className="min-h-screen w-full flex items-center justify-center bg-slate-50">
        <div className="h-10 w-10 border-4 border-slate-300 border-t-cyan-500 rounded-full animate-spin" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-screen w-full flex items-center justify-center p-6 bg-slate-50">
        <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded">
          {error}
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen w-full bg-slate-50">
      {/* Tabs */}
      <div className="border-b bg-white">
        <div className="max-w-6xl mx-auto px-4">
          <div className="flex gap-1 py-2 overflow-x-auto">
            {repos.map(repo => (
              <button
                key={repo.repositoryName}
                onClick={() => setSelectedRepo(repo.repositoryName)}
                className={`px-3 py-1 text-sm rounded cursor-pointer transition border ${
                  activeRepo?.repositoryName === repo.repositoryName
                    ? "bg-cyan-600 text-white border-cyan-600"
                    : "bg-white text-slate-700 border-slate-200 hover:bg-slate-100"
                }`}
              >
                {repo.repositoryName}
              </button>
            ))}
          </div>
        </div>
      </div>

      {/* Content */}
      <div className="max-w-6xl mx-auto p-6">
        {!activeRepo ? (
          <div className="text-slate-500">No repositories</div>
        ) : (
          <div className="space-y-3">
            {Object.entries(activeRepo.branchJobs)
              .sort(([, a], [, b]) =>
                new Date(b.registeredAt).getTime() -
                new Date(a.registeredAt).getTime()
              )
              .map(([branch, job]) => (
                <DashboardJobItem
                  key={branch}
                  branch={branch}
                  job={job}
                />
              ))}
          </div>
        )}
      </div>
    </div>
  );
}