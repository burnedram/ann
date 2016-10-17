function outfile = Lab2Task2Grapher(file)
A = csvread(file);
x = A(:, 1);
y = A(:, 2);
colors = A(:, 3);

[path, name, ~] = fileparts(file);
outfile = sprintf('%s.png', name);
outname = outfile;
if path ~= ''
    outfile = strcat([path, filesep, outfile]);
end

h = figure('Name', outname, 'NumberTitle', 'off', 'Position', [0 0 600 600]);
hold on;
scatter(x(colors == 1), y(colors == 1), 'o', 'MarkerEdgeColor', 'black', 'MarkerFaceColor', 'red');
scatter(x(colors == 2), y(colors == 2), 'o', 'MarkerEdgeColor', 'black', 'MarkerFaceColor', [0 .75 0]);
scatter(x(colors == 3), y(colors == 3), 'o', 'MarkerEdgeColor', 'black', 'MarkerFaceColor', 'blue');
title('Classification using Kohonen''s algorithm');
legend({'Class 1', 'Class 2', 'Class 3'}, 'Location', 'Best');
xlim([-0.99 20]);
ylim([-0.99 20]);
grid on;
grid minor;
saveas(h, outfile);

end