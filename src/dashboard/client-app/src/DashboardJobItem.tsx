import { AzureSyncStateBadge } from "./AzureSyncStateBadge";
import JenkinsStatusBadge from "./JenkinsStatusBadge";

type Job = {
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

function DashboardJobItem({
  branch,
  job,
}: {
  branch: string;
  job: Job;
}) {
  return (
    <div className="bg-white border border-slate-200 rounded-lg p-4 flex items-center justify-between hover:border-slate-300 transition">

      {/* LEFT */}
      <div className="space-y-1">
        <div className="font-semibold text-slate-800">
          {branch}
        </div>

        <div className="text-sm text-slate-500 flex flex-wrap items-center gap-2">

          <span className="font-mono">
            {job.commit.slice(0, 7)}
          </span>

          <span className="text-slate-400">•</span>

          <span className="text-slate-600">
            Jenkins pipeline{" "}
            {job.buildUrl ? (
              <a
                href={job.buildUrl}
                target="_blank"
                rel="noreferrer"
                className="font-mono text-cyan-700 hover:underline"
              >
                #{job.build}
              </a>
            ) : (
              <span className="font-mono">#{job.build}</span>
            )}
          </span>

          <JenkinsStatusBadge status={job.jobEvent} />
        </div>
      </div>

      <div className="flex items-end">
        <AzureSyncStateBadge
          status={job.syncStatus}
          registeredAt={job.registeredAt}
          enqueuedAt={job.enqueuedAt}
          finishedAt={job.finishedAt}
        />
      </div>
    </div>
  );
}

export default DashboardJobItem;