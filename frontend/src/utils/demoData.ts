// моки
export const DEMO_FALLBACK = import.meta.env.DEV && import.meta.env.MODE !== 'test';

export const DEMO_EMAIL = 'student@urfu.ru';

export const DEMO_PROGRESS = {
    totalLaboratories: 8,
    completedLaboratories: 5,
    earnedPoints: 64,
    totalPoints: 100,
    progressPercent: 64,
    laboratories: [
        { laboratoryId: 'demo-1', title: 'Разведка сети: Nmap и анализ портов', status: 3, earnedPoints: 10, maxPoints: 10 },
        { laboratoryId: 'demo-2', title: 'SQL-инъекции в веб-приложении', status: 2, earnedPoints: 0, maxPoints: 15 },
        { laboratoryId: 'demo-3', title: 'Перехват трафика в Wireshark', status: 3, earnedPoints: 12, maxPoints: 12 },
        { laboratoryId: 'demo-4', title: 'Повышение привилегий в Linux', status: 4, earnedPoints: 6, maxPoints: 15 },
        { laboratoryId: 'demo-5', title: 'Анализ журналов и поиск инцидента', status: 1, earnedPoints: 0, maxPoints: 10 },
    ],
};

const DEMO_NAMES = ['Анна Смирнова', 'Дмитрий Козлов', 'Вы', 'Мария Волкова', 'Артём Новиков'];

export const DEMO_LEADERBOARD = {
    currentUserRank: 3,
    items: Array.from({ length: 24 }, (_, i) => ({
        rank: i + 1,
        studentId: `demo-${i}`,
        fullName: DEMO_NAMES[i] ?? `Студент ${i + 1}`,
        earnedPoints: Math.max(92 - i * 14, 8),
        isCurrentUser: i === 2,
    })),
};
DEMO_LEADERBOARD.items[2].earnedPoints = DEMO_PROGRESS.earnedPoints;

const daysAgo = (days: number) => new Date(Date.now() - days * 86_400_000).toISOString();

export const DEMO_POSTS = [
    {
        id: 'demo-post-1',
        title: 'Открыта лабораторная «SQL-инъекции в веб-приложении»',
        content: 'Стенд доступен до конца недели. Перед стартом пройдите вводный блок про параметризованные запросы — без него флаг найти сложнее.',
        authorFullName: 'Алексей Иванов',
        category: 1,
        createdAtUtc: daysAgo(0),
    },
    {
        id: 'demo-post-2',
        title: 'CTF-тренировка в четверг, 18:00',
        content: 'Разбираем задачи прошлого квалификационного этапа. Аудитория Р-237, можно приходить со своим ноутбуком.',
        authorFullName: 'Алексей Иванов',
        category: 0,
        createdAtUtc: daysAgo(1),
    },
    {
        id: 'demo-post-3',
        title: 'Сроки сдачи отчётов перенесены',
        content: 'Отчёты по первым трём лабораторным принимаются до 3 ноября. После этой даты баллы снижаются на 20%.',
        authorFullName: 'Администрация курса',
        category: 2,
        createdAtUtc: daysAgo(3),
    },
    {
        id: 'demo-post-4',
        title: 'Обновлён стенд «Перехват трафика в Wireshark»',
        content: 'Добавили новый дамп с HTTPS-трафиком и подсказку про ключи сессии. Прогресс по работе сохранён.',
        authorFullName: 'Алексей Иванов',
        category: 1,
        createdAtUtc: daysAgo(6),
    },
];
