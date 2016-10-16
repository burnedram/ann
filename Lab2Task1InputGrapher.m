function outfile = Lab2Task1InputGrapher(filePattern, sigma)
input = csvread(sprintf(filePattern, 'input', sigma));
ordering = csvread(sprintf(filePattern, 'ordering', sigma));
convergance = csvread(sprintf(filePattern, 'convergance', sigma));

[path, name, ~] = fileparts(sprintf(filePattern, 'ordering', sigma));
outfile = sprintf('%s.png', name);
outname = outfile;
if path ~= ''
    outfile = strcat([path, filesep, outfile]);
end

h = figure('Name', outname, 'NumberTitle', 'off');
hold on;
scatter(input(:, 1), input(:, 2), '.blue');
plot(ordering(:, 1), ordering(:, 2));
plot([0 0.5 1 0], [0 1 0 0], 'black');
title(sprintf('Weights after ordering, \\sigma_0 = %d', sigma));
saveas(h, outfile);

[path, name, ~] = fileparts(sprintf(filePattern, 'convergance', sigma));
outfile = sprintf('%s.png', name);
outname = outfile;
if path ~= ''
    outfile = strcat([path, filesep, outfile]);
end

h = figure('Name', outname, 'NumberTitle', 'off');
hold on;
scatter(input(:, 1), input(:, 2), '.blue');
plot(convergance(:, 1), convergance(:, 2));
plot([0 0.5 1 0], [0 1 0 0], 'black'); 
title(sprintf('Weights after convergance, \\sigma_0 = %d', sigma));
saveas(h, outfile);
end