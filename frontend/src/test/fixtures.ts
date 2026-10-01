export const labDetail = {
    id: 'lab-1', title: 'Сетевая безопасность', shortDescription: 'Практика', description: 'Изучите сеть',
    narrative: 'Сценарий', goal: 'Найти уязвимость', environmentUrl: null, credentials: null,
    difficulty: 1, block: 'Сети', maxPoints: 10, hasFlag: false, hasExpectedFlag: false,
    isPublished: false, sortOrder: 2, deadlineAtUtc: null, hints: [], earnedPoints: 0,
    flagAlreadySubmitted: false, reportStatus: 0, allowReportUpload: true, canResubmitReport: true, report: null,
};
export const progress = { totalLaboratories: 2, completedLaboratories: 1, earnedPoints: 10, totalPoints: 20, progressPercent: 50,
    laboratories: [{ laboratoryId: 'lab-1', title: 'Сети', status: 3, earnedPoints: 10, maxPoints: 10 }] };
export const leaderboard = { currentUserRank: 1, items: [{ rank: 1, studentId: 'user-1', fullName: 'Иван Петров', earnedPoints: 10, isCurrentUser: true }] };
export const student = { id: 'user-1', fullName: 'Иван Петров', email: 'ivan@example.com', role: 0, isActive: true };
