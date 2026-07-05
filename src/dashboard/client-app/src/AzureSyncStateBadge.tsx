import dayjs from "dayjs";

type Props = {
  status: string;
  registeredAt: string;
  enqueuedAt?: string | null;
  finishedAt?: string | null;
};

export function AzureSyncStateBadge({
  status,
  registeredAt,
  enqueuedAt,
  finishedAt,
}: Props) {
  const base = "text-xs px-2 py-0.5 rounded-full border font-medium";

  const now = dayjs.utc();

  const getReferenceTime = () => {
    switch (status) {
      case "pending":
        return registeredAt;
      case "enqueued":
      case "processing":
        return enqueuedAt ?? registeredAt;
      case "completed":
      case "failed":
        return finishedAt ?? enqueuedAt ?? registeredAt;
      default:
        return registeredAt;
    }
  };

  const refTime = dayjs.utc(getReferenceTime());
  const relative = refTime.from(now);

  const color =
    status === "completed"
      ? "bg-green-100 text-green-700 border-green-200"
      : status === "failed"
      ? "bg-red-100 text-red-700 border-red-200"
      : status === "processing"
      ? "bg-cyan-100 text-cyan-700 border-cyan-200"
      : status === "enqueued"
      ? "bg-yellow-100 text-yellow-700 border-yellow-200"
      : "bg-slate-100 text-slate-600 border-slate-200";

  return (
    <div className="flex flex-col items-end leading-tight">
      <span className={`${base} ${color}`}>
        {status}
      </span>

      <span className="text-[11px] text-slate-500 mt-0.5">
        {relative}
      </span>
    </div>
  );
}
