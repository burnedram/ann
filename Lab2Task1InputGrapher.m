function outfile = Lab2Task1InputGrapher(filePattern, sigma)
input = csvread(sprintf(filePattern, 'input', sigma));
ordering = csvread(sprintf(filePattern, 'ordering', sigma));
convergance = csvread(sprintf(filePattern, 'convergance', sigma));

[path, name, ~] = fileparts(sprintf(filePattern, 'result', sigma));
outfile = sprintf('%s.png', name);
outname = outfile;
if path ~= ''
    outfile = strcat([path, filesep, outfile]);
end

h = figure('Name', outname, 'NumberTitle', 'off', 'Position', [100, 100, 1200, 600]);
h.PaperPositionMode = 'auto';

sp = subplot(1, 2, 1);
set(sp,'LooseInset',get(sp,'TightInset'));
hold on;
scatter(input(:, 1), input(:, 2), '.blue');
plot(ordering(:, 1), ordering(:, 2), 'LineWidth', 1.5);
plot([0 0.5 1 0], [0 1 0 0], 'black');
title(sprintf('Weights after ordering, \\sigma_0 = %d', sigma));

sp = subplot(1, 2, 2);
set(sp,'LooseInset',get(sp,'TightInset'));
hold on;
scatter(input(:, 1), input(:, 2), '.blue');
plot(convergance(:, 1), convergance(:, 2), 'LineWidth', 1.5);
plot([0 0.5 1 0], [0 1 0 0], 'black'); 
title(sprintf('Weights after convergance, \\sigma_0 = %d', sigma));
saveas(h, outfile);
end