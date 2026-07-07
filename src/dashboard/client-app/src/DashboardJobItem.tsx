import { Loader2, Play } from "lucide-react";
import { AzureSyncStateBadge } from "./AzureSyncStateBadge";
import { useRequeue } from "./hooks/useRequeue";
import JenkinsStatusBadge from "./JenkinsStatusBadge";
import type { Job } from "./job";

function DashboardJobItem({
    job,
}: {
    job: Job;
}) {
    const { submit, loading: requeuing, error } = useRequeue();

    const handleRequeue = async () => {
        try {
            await submit(job.name, job.build, job.commit);

            if (!error) {
                alert("Job queued successfully. It will be picked up shortly.");
            }
        } catch {
            alert("Failed to queue job. Please try again later.");
        }
    };

    return (
        <div className="flex items-center justify-between">
            {/* LEFT */}
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

            {/* RIGHT */}
            <div className="flex items-center gap-2">
                <AzureSyncStateBadge
                    status={job.syncStatus}
                    registeredAt={job.registeredAt}
                    enqueuedAt={job.enqueuedAt}
                    finishedAt={job.finishedAt}
                />

                <button
                    type="button"
                    onClick={handleRequeue}
                    disabled={requeuing}
                    title="Requeue job"
                    className="
                        flex h-10 w-10 items-center justify-center
                        rounded-full
                        border border-cyan-200
                        text-cyan-600
                        hover:bg-cyan-50
                        disabled:cursor-not-allowed
                        disabled:opacity-60
                        transition
                        cursor-pointer
                    "
                >
                    {requeuing ? (
                        <Loader2 className="h-6 w-6 animate-spin" />
                    ) : (
                        <Play className="h-6 w-6 fill-current" />
                    )}
                </button>
            </div>
        </div>
    );
}

export default DashboardJobItem;