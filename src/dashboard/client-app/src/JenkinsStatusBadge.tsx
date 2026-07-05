function JenkinsStatusBadge({ status }: { status: string }) {
    const base = "text-xs px-2 py-0.5 rounded-full border font-medium";

    switch (status.toLowerCase()) {
        case "succeeded":
            return (
                <span className={`${base} bg-green-100 text-green-700 border-green-200`}>
                    succeeded
                </span>
            );

        case "failed":
            return (
                <span className={`${base} bg-red-100 text-red-700 border-red-200`}>
                    failed
                </span>
            );

        case "aborted":
            return (
                <span className={`${base} bg-slate-200 text-slate-600 border-slate-300`}>
                    aborted
                </span>
            );

        case "started":
        default:
            return (
                <span className={`${base} bg-blue-100 text-blue-700 border-blue-200`}>
                    started
                </span>
            );
    }
}

export default JenkinsStatusBadge;