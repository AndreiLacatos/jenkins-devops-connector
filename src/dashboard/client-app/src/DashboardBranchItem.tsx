import DashboardJobItem from "./DashboardJobItem";
import { type Job } from './job';

function DashboardBranchItem({
  branch,
  jobs,
}: {
  branch: string;
  jobs: Record<string, Job>;
}) {
  return (
    <div className="bg-white border border-slate-200 rounded-lg p-4 hover:border-slate-300 transition">
      <div className="font-semibold text-slate-800 mb-2">
        {branch}
      </div>

      <div className="space-y-2">
        {Object.entries(jobs).map(([jobName, job]) => (
          <DashboardJobItem key={jobName} job={job} />
        ))}
      </div>
    </div>
  );
}

export default DashboardBranchItem;