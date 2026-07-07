export type Job = {
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
